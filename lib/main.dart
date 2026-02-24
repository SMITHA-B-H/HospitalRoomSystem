import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        backgroundColor: Colors.blueGrey,
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: const [
              Text("ROOM 101", style: TextStyle(fontSize: 40, color: Colors.white)),
              SizedBox(height: 20),
              Text("Patient: ---", style: TextStyle(fontSize: 24)),
              Text("Doctor: ---", style: TextStyle(fontSize: 24)),
              Text("Status: Available", style: TextStyle(fontSize: 24)),
            ],
          ),
        ),
      ),
    );
  }
}