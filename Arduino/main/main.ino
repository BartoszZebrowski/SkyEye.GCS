#include <SimpleFOC.h>
#include <Adafruit_BNO08x.h>

#define DEBUG 1

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
float gx, gy, gz;
float pitch, roll, yaw;


MagneticSensorI2C verticalMotorSensor = MagneticSensorI2C(AS5600_I2C);
BLDCMotor verticalMotor = BLDCMotor(7);
BLDCDriver3PWM verticalMotorDriver = BLDCDriver3PWM(6, 5, 4, 3);

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

  // command.add('T', doTarget, "target angle");
  // command.add('M',doMotor,'motor');

  _delay(1000);
}


void loop() {
  if(!DEBUG)
  {
    doCommunication();
  }

  doStabilization();

  loopIMU();

  // verticalMotor.loopFOC();
  // verticalMotor.move(verticalTargetAngle - sensorValue.un.gameRotationVector.j);

  // debugValue("velocity", verticalMotorSensor.getVelocity());
  // debugValue("angle", verticalMotorSensor.getAngle());
  
  // horizontalMotor.loopFOC();
  // horizontalMotor.move(horizontalTargetAngle);

  
  //command.run();
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
  verticalMotor.controller = MotionControlType::angle;

  // bardzo dobre nastawy
  verticalMotor.PID_velocity.P = 1.300;
  verticalMotor.PID_velocity.I = 5;
  verticalMotor.PID_velocity.D = 0;
  verticalMotor.voltage_limit = 20;
  verticalMotor.LPF_velocity.Tf = 0.01f;
  verticalMotor.P_angle.P = 10;
  verticalMotor.velocity_limit = 2;
  //verticalMotor.useMonitoring(Serial);
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
        debugValue("x", sensorValue.un.gameRotationVector.i);
        debugValue("y", sensorValue.un.gameRotationVector.j);
        debugValue("z", sensorValue.un.gameRotationVector.k);
      break;    
      case SH2_ACCELEROMETER:
        ax = sensorValue.un.accelerometer.x;
        ay = sensorValue.un.accelerometer.y;
        az = sensorValue.un.accelerometer.z;

        debugValue("ax", sensorValue.un.accelerometer.x);
        debugValue("ay", sensorValue.un.accelerometer.y);
        debugValue("az", sensorValue.un.accelerometer.z);
      break;    
      case SH2_GYROSCOPE_UNCALIBRATED:    
        gx = sensorValue.un.gyroscopeUncal.x;
        gy = sensorValue.un.gyroscopeUncal.y;
        gz = sensorValue.un.gyroscopeUncal.z;

        debugValue("gx", sensorValue.un.gyroscopeUncal.x);
        debugValue("gy", sensorValue.un.gyroscopeUncal.y);
        debugValue("gz", sensorValue.un.gyroscopeUncal.z);
      break;
  }


// float qw = sensorValue.un.rotationVector.real;
// float qx = sensorValue.un.rotationVector.i;
// float qy = sensorValue.un.rotationVector.j;
// float qz = sensorValue.un.rotationVector.k;

// // pitch (x-axis rotation)
// float sinp = 2.0f * (qw * qx + qy * qz);
// float cosp = 1.0f - 2.0f * (qx * qx + qy * qy);
// float pitch = atan2(sinp, cosp);

// // roll (y-axis rotation)
// float sinr = 2.0f * (qw * qy - qz * qx);
// float roll = asin(sinr);

// // yaw (z-axis rotation)
// float siny = 2.0f * (qw * qz + qx * qy);
// float cosy = 1.0f - 2.0f * (qy * qy + qz * qz);
// float yaw = atan2(siny, cosy);


}

void setReportsIMU()
{
  if (!bno085.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    print("Could not enable gyroscope");
  }

  bno085.enableReport(SH2_ACCELEROMETER);
  bno085.enableReport(SH2_GYROSCOPE_UNCALIBRATED);
}

void doStabilization(){
  calculateAngle();

  
}

void calculateAngle(){
  pitch = atan2f(az, ax);
  roll = atan2f(ax, ay) - 1.6;
  yaw = atan2f(az, ay);

  debugValue("Pitch", pitch);
  debugValue("Roll", roll);
  debugValue("Yaw", yaw);
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

