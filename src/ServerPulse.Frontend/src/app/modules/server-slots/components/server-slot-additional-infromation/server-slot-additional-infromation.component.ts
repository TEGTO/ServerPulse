import { Component } from '@angular/core';
import { ServerSlotDialogManager } from '../../index';

@Component({
  selector: 'server-slot-additional-info',
  templateUrl: './server-slot-additional-infromation.component.html',
  styleUrl: './server-slot-additional-infromation.component.scss'
})
export class ServerSlotAdditionalInfromationComponent {
  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  constructor(
    private readonly dialogManager: ServerSlotDialogManager
  ) { }

  openDetailMenu(serializedEvent: string) {
    let test: string =
      "{\"Id\": \"someid\",\"Key\": \"ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff111\",\"CreationDateUTC\": \"2024-08-07T14:30:45.4656254Z\",\"Endpoint\": \"/api/v1/resource\",\"Method\": \"GET\",\"StatusCode\": 200,\"Duration\":\"00:00:00.1500000\",\"TimestampUTC\": \"2024-08-07T14:30:45.4656254Z\"}";
    this.dialogManager.openCustomEventDetails(test);
  }
}
