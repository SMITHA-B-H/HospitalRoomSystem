import { Component } from '@angular/core';
import { Room } from '../../models/room.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent {

  rooms: Room[] = [
    { id: 1, roomNumber: '101', status: 'Available' },
    { id: 2, roomNumber: '102', status: 'Occupied' },
    { id: 3, roomNumber: '103', status: 'Available' }
  ];

  totalRooms = 0;
  availableRooms = 0;
  occupiedRooms = 0;

  constructor() {
    this.calculateStats();
  }

  calculateStats() {
    this.totalRooms = this.rooms.length;
    this.availableRooms =
      this.rooms.filter(r => r.status === 'Available').length;
    this.occupiedRooms =
      this.rooms.filter(r => r.status === 'Occupied').length;
  }
}