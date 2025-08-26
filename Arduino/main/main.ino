#include <SimpleFOC.h>
#include <Adafruit_BNO08x.h>
#include <math.h>
#include <quaternion_type.h>
#include <PID_v1.h>

#define DEG2RAD(x)  ((x)*PI/180.0f)
#define DEBUG 1

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

void configureVerticalMotor();
void configureHorizontalMotor();

void loopIMU();
void setReportsIMU();

void doCommunication();
void handleCommand(String parts[], int count);
int splitCommand(String input, String parts[], char delimiter = ';');

void print(String message);
void debugValue(String name, float value);

enum RemoteVariableType{
  Ping = 0,
  WorkingMode,
  TargetHorizontalAngle,
  TargetVerticalAngle,
  ActualHorizontaAngle,
  ActualVerticalAngle,
  ZoomValue,
};

static Quaternion imuQ;

const unsigned long interval = 1000;
unsigned long lastRequestTime = 0;
int currentVariable = 0;

Adafruit_BNO08x bno085(-1);
sh2_SensorValue_t sensorValue;
unsigned long startTime;

MagneticSensorI2CConfig_s AS5600_I2C_2 = {
  .chip_address = 0x66,
  .bit_resolution = 12,
  .angle_register = 0x0C,
  .msb_mask = 0x0F,
  .msb_shift = 8,
  .lsb_mask = 0xFF,
  .lsb_shift = 0
};

MagneticSensorI2C verticalMotorSensor = MagneticSensorI2C(AS5600_I2C);
BLDCMotor verticalMotor = BLDCMotor(7);
BLDCDriver3PWM verticalMotorDriver = BLDCDriver3PWM(6, 5, 4, 3);
float verticalError = 0;

MagneticSensorI2C horizontalMotorSensor = MagneticSensorI2C(AS5600_I2C_2);
BLDCMotor horizontalMotor = BLDCMotor(7);
BLDCDriver3PWM horizontalMotorDriver = BLDCDriver3PWM(10, 9, 8, 7);
float horizontalError = 0;

float verticalTargetAngle = 0;
float horizontalTargetAngle = 0;

Commander command = Commander(Serial);

void doTarget(char* cmd) { command.scalar(&verticalTargetAngle, cmd); }
void doMotor(char* cmd) { command.motor(&verticalMotor, cmd); } 

void setup() {
  Wire.setClock(400000);
  Serial.begin(115200);

  configureIMU();
  configureVerticalMotor();
  configureHorizontalMotor();

  command.add('T', doTarget, "target angle");
  command.add('M', doMotor, 'motor');

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

  command.run();
}

void configureVerticalMotor(){
  verticalMotorSensor.init();
  verticalMotor.linkSensor(&verticalMotorSensor);
  verticalMotorDriver.voltage_power_supply = 20;

  verticalMotorDriver.init();
  verticalMotor.linkDriver(&verticalMotorDriver);

  verticalMotor.foc_modulation = FOCModulationType::SpaceVectorPWM;
  verticalMotor.controller = MotionControlType::angle;

  verticalMotor.PID_velocity.P = 2.3;
  verticalMotor.PID_velocity.I = 1;
  verticalMotor.PID_velocity.D = 0.01;
  verticalMotor.PID_velocity.output_ramp = 1000;
  verticalMotor.voltage_limit = 12;
  verticalMotor.LPF_velocity.Tf = 0.002f;
  verticalMotor.P_angle.P = 1.0;
  verticalMotor.P_angle.I = 1;
  verticalMotor.P_angle.D = 0.01;
  verticalMotor.P_angle.output_ramp = 1000;
  verticalMotor.LPF_angle.Tf = 0;
  verticalMotor.velocity_limit = 100;
  verticalMotor.useMonitoring(Serial);
  verticalMotor.pole_pairs = 7;
  verticalMotor.init();
  verticalMotor.initFOC();

  print(F("Vertical motor ready."));
}

void configureHorizontalMotor(){
  horizontalMotorSensor.init();
  horizontalMotor.linkSensor(&horizontalMotorSensor);

  horizontalMotorDriver.voltage_power_supply = 20;
  horizontalMotorDriver.init();
  horizontalMotor.linkDriver(&horizontalMotorDriver);

  horizontalMotor.foc_modulation = FOCModulationType::SpaceVectorPWM;
  horizontalMotor.controller = MotionControlType::angle;

  verticalMotor.PID_velocity.P = 2.6;
  verticalMotor.PID_velocity.I = 1.8;
  verticalMotor.PID_velocity.D = 0.01;
  horizontalMotor.PID_velocity.output_ramp = 1000;
  horizontalMotor.voltage_limit = 8;
  horizontalMotor.voltage_sensor_align = 4;
  horizontalMotor.LPF_velocity.Tf = 0.002f;
  horizontalMotor.P_angle.P = 1;
  horizontalMotor.P_angle.I = 1;
  horizontalMotor.P_angle.D = 0.01;
  horizontalMotor.P_angle.output_ramp = 1000;
  horizontalMotor.LPF_angle.Tf = 0;
  horizontalMotor.velocity_limit = 100;
  horizontalMotor.useMonitoring(Serial);
  horizontalMotor.init();
  horizontalMotor.initFOC();
  horizontalMotor.pole_pairs = 7;

  print(F("Horizontal motor ready."));
}

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
  }
}

void setReportsIMU()
{
  if (!bno085.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    print("Could not enable SH2_GAME_ROTATION_VECTOR");
  }

  bno085.enableReport(SH2_GAME_ROTATION_VECTOR, 2000);
}

void doStabilization() {
  static const Quaternion rot90 = Quaternion::fromEulerDeg(0,90,0);
  Quaternion rotated = rot90 * imuQ;

  float roll, pitch, yaw;
  rotated.toEuler(roll, pitch, yaw);

  verticalError = -pitch;
  horizontalError = yaw;

  verticalMotor.move(verticalError - verticalMotorSensor.getSensorAngle());
  horizontalMotor.move(horizontalError -horizontalMotorSensor.getSensorAngle());

  verticalMotor.loopFOC();
  horizontalMotor.loopFOC();
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

  if (command == "3" && count == 2) {
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
