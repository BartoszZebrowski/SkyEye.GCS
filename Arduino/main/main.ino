#include <SimpleFOC.h>
#include <Adafruit_BNO08x.h>
#include <math.h>
#include <quaternion_type.h>

#define DEG2RAD(x)  ((x)*PI/180.0f)


struct Quaternion {
  float w,x,y,z;
  
  constexpr Quaternion(float _w=1,float _x=0,float _y=0,float _z=0)
    :w(_w),x(_x),y(_y),z(_z){}
  
  inline void norm()
  {
     float n=sqrtf(w*w+x*x+y*y+z*z); 
     if(n){w/=n;x/=n;y/=n;z/=n;} 
  }
  
  inline Quaternion operator*(const Quaternion& q) const {
    return { w*q.w - x*q.x - y*q.y - z*q.z,
             w*q.x + x*q.w + y*q.z - z*q.y,
             w*q.y - x*q.z + y*q.w + z*q.x,
             w*q.z + x*q.y - y*q.x + z*q.w }; 
  }
  
  static Quaternion fromEulerDeg(float rD,float pD,float yD){
    const float r=DEG2RAD(rD)*0.5f, p=DEG2RAD(pD)*0.5f, y=DEG2RAD(yD)*0.5f;
    const float cr=cosf(r), sr=sinf(r), cp=cosf(p), sp=sinf(p), cy=cosf(y), sy=sinf(y);
    return { cr*cp*cy + sr*sp*sy,  sr*cp*cy - cr*sp*sy,
             cr*sp*cy + sr*cp*sy,  cr*cp*sy - sr*sp*cy };
  }
  
  void toEuler(float& roll,float& pitch,float& yaw) const {
    roll  = atan2f(2*(w*x+y*z), 1-2*(x*x+y*y));
    const float s = 2*(w*y - z*x);
    pitch = fabsf(s)>=1 ? copysignf(PI/2,s) : asinf(s);
    yaw   = atan2f(2*(w*z+x*y), 1-2*(y*y+z*z));
  }
};

static Quaternion imuQ;


#define DEBUG 0

#define IMU_ENCODER_PITCH_OFFSET 2.69

enum RemoteVariableType{
  Ping = 0,
  WorkingMode,
  TargetHorizontalAngle,
  TargetVerticalAngle,
  ActualHorizontaAngle,
  ActualVerticalAngle,
  ZoomValue,
};
//komunikacja
const unsigned long interval = 1000;
unsigned long lastRequestTime = 0;
int currentVariable = 0;


void configureVerticalMotor();
//void configureHorizontalMotor();

void loopIMU();
void setReportsIMU();

void doCommunication();
void handleCommand(String parts[], int count);
int splitCommand(String input, String parts[], char delimiter = ';');

void print(String message);
void debugValue(String name, float value);

Adafruit_BNO08x bno085(-1);
sh2_SensorValue_t sensorValue;

float ax, ay, az;
float aPitch, aRoll;

float gx, gy, gz;
float gRoll, gPitch, gYaw;

float mx, my, mz;
float mYaw;

float alpha = 0.85;

float i, k, j, real;

unsigned long startTime;

float pitch, roll, yaw;

float dt;
float previousTime;

float targetEncoderAngle;

MagneticSensorI2C verticalMotorSensor = MagneticSensorI2C(AS5600_I2C);
BLDCMotor verticalMotor = BLDCMotor(7);
BLDCDriver3PWM verticalMotorDriver = BLDCDriver3PWM(6, 5, 4, 3);
float verticalError = 0;

// MagneticSensorI2C horizontalMotorSensor = MagneticSensorI2C(AS5600_I2C);
// BLDCMotor horizontalMotor = BLDCMotor(7);
// BLDCDriver3PWM horizontalMotorDriver = BLDCDriver3PWM(6, 5, 4, 3);

float verticalTargetAngle = 0;
float horizontalTargetAngle = 0;

Commander command = Commander(Serial);

void doTarget(char* cmd) { command.scalar(&verticalTargetAngle, cmd); }
void doMotor(char* cmd) { command.motor(&verticalMotor, cmd); } 

void setup() {
  Wire.setClock(400000);
  Serial.begin(115200);
  //SimpleFOCDebug::enable(&Serial);

  configureIMU();
  configureVerticalMotor();

  command.add('T', doTarget, "target angle");
  command.add('M',doMotor,'motor');

  _delay(1000);
  startTime = micros();
}


void loop() {
  if(!DEBUG)
  {
    doCommunication();
  }

  doStabilization();
  loopIMU();

  verticalMotor.loopFOC();
  verticalMotor.move(-verticalError * 10);


  debugValue("velocity", verticalMotorSensor.getVelocity());  
  debugValue("verticalError", -verticalError);  
  // debugValue("angle", verticalMotorSensor.getAngle());
  
  // horizontalMotor.loopFOC();
  // horizontalMotor.move();

  debugValue("encoder", verticalMotorSensor.getSensorAngle());
  // debugValue("ax", ax);
  // debugValue("ay", ay);
  // debugValue("az", az);
  // debugValue("gx", gx);
  // debugValue("gy", gy);
  // debugValue("gz", gz);
  // debugValue("mx", mx);
  // debugValue("my", my);
  // debugValue("mz", mz);
  // debugValue("roll", roll);
  // debugValue("pitch", pitch);
  // debugValue("yaw", yaw);  
  
  // debugValue("aRoll", aRoll);
  // debugValue("aPitch", aPitch);
  // debugValue("mYaw", mYaw);
  // debugValue("gRoll", gRoll);
  // debugValue("gPitch", gPitch);
  // debugValue("gYaw", gYaw);


  // Serial.print(micros()- startTime);
  // Serial.print(";");
  // Serial.print(verticalMotorSensor.getSensorAngle()); Serial.print(";");
  // Serial.print(ax); Serial.print(";");
  // Serial.print(ay); Serial.print(";");
  // Serial.print(az); Serial.print(";");
  // Serial.print(gx); Serial.print(";");
  // Serial.print(gy); Serial.print(";");
  // Serial.print(gz); Serial.print(";");
  // Serial.print(mx); Serial.print(";");
  // Serial.print(my); Serial.print(";");  
  // Serial.print(mz); Serial.print(";");
  // Serial.print(i); Serial.print(";");
  // Serial.print(k); Serial.print(";");
  // Serial.print(j); Serial.print(";");
  // Serial.println(real);
  // delay(25);

  command.run();

}

void configureVerticalMotor(){
  //sensor
  verticalMotorSensor.init();
  verticalMotor.linkSensor(&verticalMotorSensor);
  //driver
  verticalMotorDriver.voltage_power_supply = 20;
  verticalMotorDriver.init();
  verticalMotor.linkDriver(&verticalMotorDriver);

  //motor
  verticalMotor.foc_modulation = FOCModulationType::SpaceVectorPWM;
  verticalMotor.controller = MotionControlType::velocity;

  // bardzo dobre nastawy
  verticalMotor.PID_velocity.P = 2;
  verticalMotor.PID_velocity.I = 1;
  verticalMotor.PID_velocity.D = 0.01;
  verticalMotor.PID_velocity.output_ramp = 1000;
  verticalMotor.voltage_limit = 20;
  verticalMotor.LPF_velocity.Tf = 0.002f;
  verticalMotor.P_angle.P = 2;
  verticalMotor.velocity_limit = 100;
  verticalMotor.useMonitoring(Serial);
  verticalMotor.init();
  verticalMotor.initFOC();

  print(F("Vertical motor ready."));
}

// void configureHorizontalMotor(){
//   //sensor
//   horizontalMotorSensor.init();
//   horizontalMotor.linkSensor(&verticalMotorSensor);

//   //driver
//   horizontalMotorDriver.voltage_power_supply = 12;
//   horizontalMotorDriver.init();
//   horizontalMotor.linkDriver(&verticalMotorDriver);

//   //motor
//   horizontalMotor.foc_modulation = FOCModulationType::SpaceVectorPWM;
//   horizontalMotor.controller = MotionControlType::angle;
//   horizontalMotor.PID_velocity.P = 0.2f;
//   horizontalMotor.PID_velocity.I = 20;
//   horizontalMotor.PID_velocity.D = 0;
//   horizontalMotor.voltage_limit = 6;
//   horizontalMotor.LPF_velocity.Tf = 0.01f;
//   horizontalMotor.P_angle.P = 20;
//   horizontalMotor.velocity_limit = 20;
//   horizontalMotor.useMonitoring(Serial);
//   horizontalMotor.init();
//   horizontalMotor.initFOC();

//   print(F("Horisontal motor ready."));
// }

void configureIMU(){
  if (!bno085.begin_I2C()) {
    print("Failed to find BNO08x chip");
    while (1) {
      delay(10);
    }
  }
  print("BNO08x Found!");

  setReportsIMU();
}

void loopIMU(){
  if (bno085.wasReset()) {
    print("sensor was reset ");
    setReportsIMU();
  }

  if (!bno085.getSensorEvent(&sensorValue)) {
    return;
  }

  switch (sensorValue.sensorId) {
    case SH2_GAME_ROTATION_VECTOR: 
        imuQ.w = sensorValue.un.gameRotationVector.real;
        imuQ.x = sensorValue.un.gameRotationVector.i;
        imuQ.y = sensorValue.un.gameRotationVector.j;
        imuQ.z = sensorValue.un.gameRotationVector.k;
        imuQ.norm();
      break;
      // case SH2_ACCELEROMETER:
      //   ax = sensorValue.un.accelerometer.x;
      //   ay = sensorValue.un.accelerometer.y;
      //   az = sensorValue.un.accelerometer.z;
      // break;    
      // case SH2_GYROSCOPE_CALIBRATED:    
      //   gx = sensorValue.un.gyroscope.x;
      //   gy = sensorValue.un.gyroscope.y;
      //   gz = sensorValue.un.gyroscope.z;
      // break;
      // case SH2_MAGNETIC_FIELD_CALIBRATED:    
      //   mx = sensorValue.un.magneticField.x;
      //   my = sensorValue.un.magneticField.y;
      //   mz = sensorValue.un.magneticField.z;
      // break;
  }
}

void setReportsIMU()
{
  if (!bno085.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    print("Could not enable SH2_GAME_ROTATION_VECTOR");
  }
  if (!bno085.enableReport(SH2_ACCELEROMETER)) {
    print("Could not enable SH2_ACCELEROMETER");
  }
  if (!bno085.enableReport(SH2_GYROSCOPE_CALIBRATED)) {
    print("Could not enable SH2_GYROSCOPE_CALIBRATED");
  }
  if (!bno085.enableReport(SH2_GYROSCOPE_UNCALIBRATED)) {
    print("Could not enable SH2_GYROSCOPE_UNCALIBRATED");
  }
  if (!bno085.enableReport(SH2_MAGNETIC_FIELD_CALIBRATED)) {
    print("Could not enable SH2_MAGNETIC_FIELD_CALIBRATED");
  }
  if (!bno085.enableReport(SH2_MAGNETIC_FIELD_UNCALIBRATED)) {
    print("Could not enable SH2_MAGNETIC_FIELD_UNCALIBRATED");
  }

  bno085.enableReport(SH2_GAME_ROTATION_VECTOR, 2000);
}

void doStabilization(){
  // calculateDt();
  // calculateAcceleratorAngle();
  // calculateMagnetometrAngle();
  // calculateGyroscopeAngle();
  // complementaryFilter();
  createQuaternions();  
 
  
}

void calculateDt(){
  unsigned long now = millis();
  dt = (now - previousTime) / 1000.0;
  previousTime = now;
}

void calculateAcceleratorAngle(){
  aPitch = atan2(-ax, sqrt(ay * ay + az * az));
  aRoll = atan2(ay, az);
}

void calculateMagnetometrAngle(){
  float mxCorrection = (mx * sin(aRoll) * sin(aPitch)) + (my * cos(aRoll)) - (mz * sin(aRoll) * cos(aPitch));
  float myCorrection = (mx * cos(aPitch)) + (mz * sin(aPitch));
  mYaw = atan2(-myCorrection, mxCorrection);
}

void calculateGyroscopeAngle(){
  gRoll = gRoll + gx * dt;
  gPitch = gPitch + gy * dt;
  gYaw = gYaw + gz * dt;
}

void complementaryFilter(){
  roll = alpha * gRoll + (1- alpha) * aRoll;
  pitch = alpha * gPitch + (1- alpha) * aPitch;
  yaw = alpha * gYaw + (1- alpha) * mYaw;
}

void createQuaternions() {

  static const Quaternion rot90 = Quaternion::fromEulerDeg(0,90,0);
  Quaternion rotated = rot90 * imuQ; rotated.norm();

  float r,p,y; 
  rotated.toEuler(r,p,y);
  verticalError = p + verticalTargetAngle;

  // debugValue("verticalTargetAngle", verticalTargetAngle);
  // debugValue("verticalMotorSensor", verticalMotorSensor.getSensorAngle());
  // debugValue("imuPitch", imuPitch);
  // debugValue("targetEncoderAngle", targetEncoderAngle);

  debugValue("p", p);
  // debugValue("x", roteatedImu.x);
  // debugValue("y", roteatedImu.y);
  // debugValue("z", roteatedImu.z);
}

void doCommunication(){
  unsigned long now = millis();
  String response;
  String splitedResponse[2];

  if (now - lastRequestTime >= interval) {
    sendRequest((RemoteVariableType)currentVariable);
    currentVariable++;

    if (currentVariable > ZoomValue) {
      currentVariable = 0;
    }

    lastRequestTime = now;

    if (Serial.available()) {
      response = Serial.readStringUntil('\n');
      splitCommand(response, splitedResponse);
      handleCommand(splitedResponse, 2);
    }
  }
}

void sendRequest(RemoteVariableType variable) {
  String message = String((int)variable) + ";0;0\n";
  Serial.print(message);
}

void handleCommand(String parts[], int count) {
  String command = parts[0];

  if (command == "Ping") {
    print("Pong");
  } 
  else if (command == "3" && count == 2) {
    verticalTargetAngle = parts[1].toFloat();
    print("Ok");
  }  
  else if (command == "2" && count == 2) {
    horizontalTargetAngle = parts[1].toFloat();
    print("Ok");
  }
  else {
    print("ERR;UNKNOWN_CMD");
  }
}

int splitCommand(String input, String parts[], char delimiter = ';') {
  int startIndex = 0;
  int partIndex = 0;
  
  for (int i = 0; i <= input.length(); i++) {
    if (input.charAt(i) == delimiter || i == input.length()) {
      parts[partIndex] = input.substring(startIndex, i);
      partIndex++;
      startIndex = i + 1;
    }
  }
  
  return partIndex;
}

void print(String message){
  if(!DEBUG) 
    return; 

  Serial.println(message);
}

void debugValue(String name, float value){
  if(!DEBUG) 
    return; 

  Serial.print(">");
  Serial.print(name);
  Serial.print(":");
  Serial.println(value);
}

static inline float wrapPi(float a) {
    while (a <= -PI) a += 2 * PI;
    while (a >   PI) a -= 2 * PI;
    return a;              // wynik w zakresie (−π, π]
}

static inline float wrap2Pi(float a) {
    while (a < 0.0f)     a += 2.0f * PI;
    while (a >= 2.0f*PI) a -= 2.0f * PI;
    return a;
}
