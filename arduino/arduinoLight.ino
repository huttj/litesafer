
int onPin = 6;
int offPin = 7;

void setup() {
  Serial.begin(9600);
  pinMode(onPin, OUTPUT);
  pinMode(offPin, OUTPUT);
  digitalWrite(offPin, HIGH);
}

void set(bool on) {
  if (on) {
    digitalWrite(onPin, HIGH);
  } else {
    digitalWrite(onPin, LOW);
  }
}

int freq = -1;

void loop() {

  while (Serial.available()) {
    char inChar = (char)Serial.read();
    if (inChar == '-')
    {
      freq = -1;
      set(false);
    }
    else
      freq = (inChar - '0') * 100;
    Serial.print(freq);
  }

  if (!Serial.available() && freq >= 0) {
    if (freq > 0) {
      set(true);
      delay(freq);
      set(false);
      delay(freq);
    }
    else if (freq == 0) {
      set(true);
    }
  }

}

