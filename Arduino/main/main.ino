#include <SimpleFOC.h>
#include <Adafruit_BNO08x.h>
#include <math.h>
#include <quaternion_type.h>



class Quaternion {
public:
    float w, x, y, z;   // składowe (skalarny + wektor)

    /* --- Konstruktory --------------------------------------------------- */
    Quaternion(float w = 1.0f, float x = 0.0f, float y = 0.0f, float z = 0.0f)
        : w(w), x(x), y(y), z(z) {}

    /* --- Operatory arytmetyczne ---------------------------------------- */
    inline Quaternion operator+(const Quaternion& q) const {
        return Quaternion(w + q.w, x + q.x, y + q.y, z + q.z);
    }

    inline Quaternion operator-(const Quaternion& q) const {
        return Quaternion(w - q.w, x - q.x, y - q.y, z - q.z);
    }

    inline Quaternion operator*(float s) const {
        return Quaternion(w * s, x * s, y * s, z * s);
    }

    inline Quaternion operator/(float s) const {
        return Quaternion(w / s, x / s, y / s, z / s);
    }

    // Iloczyn Hamiltona – łączenie obrotów
    inline Quaternion operator*(const Quaternion& q) const {
        return Quaternion(
            w * q.w - x * q.x - y * q.y - z * q.z,
            w * q.x + x * q.w + y * q.z - z * q.y,
            w * q.y - x * q.z + y * q.w + z * q.x,
            w * q.z + x * q.y - y * q.x + z * q.w
        );
    }

    /* --- Właściwości ---------------------------------------------------- */
    inline float magnitude() const {
        return sqrtf(w * w + x * x + y * y + z * z);
    }

    inline void normalize() {
        float m = magnitude();
        if (m == 0) return;
        w /= m; x /= m; y /= m; z /= m;
    }

    inline Quaternion conjugate() const {
        return Quaternion(w, -x, -y, -z);
    }

    inline Quaternion inverse() const {
        float m2 = w * w + x * x + y * y + z * z;
        if (m2 == 0) return Quaternion();
        return conjugate() / m2;
    }

    /* --- Konwersje ------------------------------------------------------ */
    // Z kątów Eulera (roll, pitch, yaw) na kwaternion
    static inline Quaternion fromEuler(float roll, float pitch, float yaw, bool degrees = true) {
        if (degrees) {
            const float d2r = PI / 180.0f;
            roll  *= d2r;
            pitch *= d2r;
            yaw   *= d2r;
        }

        const float cy = cosf(yaw * 0.5f);
        const float sy = sinf(yaw * 0.5f);
        const float cp = cosf(pitch * 0.5f);
        const float sp = sinf(pitch * 0.5f);
        const float cr = cosf(roll * 0.5f);
        const float sr = sinf(roll * 0.5f);

        return Quaternion(
            cr * cp * cy + sr * sp * sy,
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy
        );
    }

    // Z kwaternionu na kąty Eulera (roll, pitch, yaw)
    inline void toEuler(float& roll, float& pitch, float& yaw, bool degrees = true) const {
        /* ROLL  */
        const float sinr_cosp = 2.0f * (w * x + y * z);
        const float cosr_cosp = 1.0f - 2.0f * (x * x + y * y);
        roll = atan2f(sinr_cosp, cosr_cosp);

        /* PITCH */
        const float sinp = 2.0f * (w * y - z * x);
        if (fabsf(sinp) >= 1.0f)
            pitch = copysignf(PI / 2.0f, sinp);     // gimbal‑lock
        else
            pitch = asinf(sinp);

        /* YAW   */
        const float siny_cosp = 2.0f * (w * z + x * y);
        const float cosy_cosp = 1.0f - 2.0f * (y * y + z * z);
        yaw = atan2f(siny_cosp, cosy_cosp);

        if (degrees) {
            const float r2d = 180.0f / PI;
            roll  *= r2d;
            pitch *= r2d;
            yaw   *= r2d;
        }
    }

    /* --- Dodatki -------------------------------------------------------- */
    // Obrót wektora 3‑D (x, y, z) w miejscu
    inline void rotateVector(float& vx, float& vy, float& vz) const {
        Quaternion v(0.0f, vx, vy, vz);
        Quaternion res = (*this) * v * this->inverse();
        vx = res.x; vy = res.y; vz = res.z;
    }
};


#define DEBUG 1



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
  Serial.begin(115200);
  //SimpleFOCDebug::enable(&Serial);

  configureIMU();
  configureVerticalMotor();
  //configureHorizontalMotor();

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
  verticalMotor.move(-targetEncoderAngle);


  debugValue("velocity", verticalMotorSensor.getVelocity());  
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
  verticalMotor.sensor_direction = Direction::CCW;
  //driver
  verticalMotorDriver.voltage_power_supply = 20;
  verticalMotorDriver.init();
  verticalMotor.linkDriver(&verticalMotorDriver);

  //motor
  verticalMotor.foc_modulation = FOCModulationType::SpaceVectorPWM;
  verticalMotor.controller = MotionControlType::velocity;

  // bardzo dobre nastawy
  verticalMotor.PID_velocity.P = 1.3;
  verticalMotor.PID_velocity.I = 1;
  verticalMotor.PID_velocity.D = 0.01;
  verticalMotor.PID_velocity.output_ramp = 1000;
  verticalMotor.voltage_limit = 20;
  verticalMotor.LPF_velocity.Tf = 0.001f;
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
        i = sensorValue.un.gameRotationVector.i;
        j = sensorValue.un.gameRotationVector.j;
        k = sensorValue.un.gameRotationVector.k;
        real = sensorValue.un.gameRotationVector.real;
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

  Quaternion imuQ(real, i, j, k);
  imuQ.normalize(); 
  
  Quaternion rot90 = Quaternion::fromEuler(0.0f, 90.0f, 0.0f, true);
  Quaternion roteatedImu = rot90 * imuQ;

  float imuRoll, imuPitch, imuYaw;
  roteatedImu.toEuler(imuRoll, imuPitch, imuYaw, false);
  targetEncoderAngle = imuPitch + verticalTargetAngle;

  debugValue("verticalTargetAngle", verticalTargetAngle);
  debugValue("verticalMotorSensor", verticalMotorSensor.getSensorAngle());
  debugValue("imuPitch", imuPitch);
  debugValue("targetEncoderAngle", targetEncoderAngle);

  debugValue("w", roteatedImu.w);
  debugValue("x", roteatedImu.x);
  debugValue("y", roteatedImu.y);
  debugValue("z", roteatedImu.z);
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
