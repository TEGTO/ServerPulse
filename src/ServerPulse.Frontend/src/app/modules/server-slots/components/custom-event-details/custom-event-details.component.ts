import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-custom-event-details',
  templateUrl: './custom-event-details.component.html',
  styleUrl: './custom-event-details.component.scss'
})
export class CustomEventDetailsComponent implements OnInit {
  public jsonData: { key: string, value: any }[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) private serializedEvent: string
  ) { }

  ngOnInit(): void {
    try {
      const parsedJson = JSON.parse(this.serializedEvent);
      this.jsonData = Object.keys(parsedJson).map(key => ({
        key: key,
        value: parsedJson[key]
      }));
    } catch (error) {
      console.error('Invalid JSON format', error);
    }
  }
}