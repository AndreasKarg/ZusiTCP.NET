#include <Servo.h>         // Damit wir auch Servos ansteuern können.
Servo servoHLL;            // So heißt der Servo für die Hauptluftleitung
Servo servoHLB;            // So heißt der Servo für den Hauptluftbehälter
Servo servoBZD;            // So heißt der Servo für den Bremszylinderdruck
byte  inputByte[2];        // [0] gibt das Ziel, [1] den Wert an
void setup()
{
  //servoHLL.attach(x);      // Den 3 Servos wird der Pin zugewiesen, an dem sie angeschlossen sind    
  //servoHLB.attach(x);
  servoBZD.attach(5);

  Serial.begin(9600);      // Der serial port soll mit 9600 bps starten
  pinMode(3, OUTPUT);      // PWM-Ausgang für einen Tacho Pin 3 sein
  pinMode(7, OUTPUT);      // LM "Sifa" soll an Pin 7
  pinMode(6, OUTPUT);      // -nicht benutzt-
  pinMode(9, OUTPUT);      // LM "Bef. 40" soll an Pin 9
  pinMode(11, OUTPUT);     // LM "500Hz" soll an Pin 11
  pinMode(13, OUTPUT);     // LM "1000Hz" soll an Pin 13
  pinMode(8, OUTPUT);      // LM "O" soll an Pin 8
  pinMode(10, OUTPUT);     // LM "M" soll an Pin 10
  pinMode(12, OUTPUT);     // LM "U" soll an Pin 12
}

void loop()
{
  if (inputByte[0]=='w'){                    // Wenn wir an erster Stelle ein "w" empfangen, möchte der PC wissen, ob wir wirklich
    Serial.println("I am a drivers desk");   // der Arduino  für das Fahrpult sind. Das bestätigen wir mit "I am a drivers desk""
    if (inputByte[1]=='w'){                  // Wenn an zweiter Stelle ebenfalls ein "w" kommt, sollen wir akustisch quitieren
      tone(5, 1000, 100);
      delay(200);
      tone(5, 1000, 100);
    }
  }
  if (inputByte[0] == 'V'){                  // "V" für Geschwindigkeit
    analogWrite(3,inputByte[1]);             // Wir geben den Wert von 0 - 255 PWM an Pin 6 aus
  }
  if (inputByte[0] == 'P'){                  // "P" für PZB, stellvertretend für alle Leuchtmelder
    LmPZB(inputByte[1]);                     // Wir geben das Byte an eine Funktion, die die 8 Bits je einem Digitalausgang zuordnet
  }
  if (inputByte[0] == 'L'){                  // "L" für Hauptluftleitung
    servoHLL.write(inputByte[1]);            // Wir weisen dem entsprechenden Servoobjekt seinen Wert zu
  }
  if (inputByte[0] == 'H'){                  // "H" für Hauptluftbehälter
    servoHLB.write(inputByte[1]);            // Wir weisen dem entsprechenden Servoobjekt seinen Wert zu
  }
  if (inputByte[0] == 'B'){                  // "B" für Bremszylinder
    servoBZD.write(inputByte[1]);            // Wir weisen dem entsprechenden Servoobjekt seinen Wert zu
  }
}

void serialEvent() {                        // Es sind uns Daten vom PC zugespielt worden
  for (byte i=0; i<2;i++){
    inputByte[i]=Serial.read();             // Wir weisen inputByte[0] Das Ziel und inputByte[1] seinen Wert zu
    delay(3);                               // Da es seine Zeit braucht, bis der PC alles sendet, warten wir zwischen den Bytes etwas ab
  }
  Serial.flush();                           // Falls aus irgend einem Grund nun noch Datan im Lesepuffer sind, löschen wir diese jetzt
}

void LmPZB(byte LM){
  if ((LM&8) > 0){digitalWrite(8, HIGH);}//U
  if ((LM&8) == 0){digitalWrite(8, LOW);}
  if ((LM&1) > 0){digitalWrite(10, HIGH);}//M
  if ((LM&1) == 0){digitalWrite(10, LOW);}
  if ((LM&2) > 0){digitalWrite(12, HIGH);}//O
  if ((LM&2) == 0){digitalWrite(12, LOW);}
  if ((LM&4) > 0){digitalWrite(9, HIGH);}//Bef
  if ((LM&4) == 0){digitalWrite(9, LOW);}
  if ((LM&16) > 0){digitalWrite(11, HIGH);}//500
  if ((LM&16) == 0){digitalWrite(11, LOW);}
  if ((LM&32) > 0){digitalWrite(13, HIGH);}//1000
  if ((LM&32) == 0){digitalWrite(13, LOW);}
  if ((LM&64) > 0){digitalWrite(7, HIGH);}//Sifa
  if ((LM&64) == 0){digitalWrite(7, LOW);}
  if ((LM&128) > 0){digitalWrite(6, HIGH);}//nicht benutzt
  if ((LM&128) == 0){digitalWrite(6, LOW);}
}
