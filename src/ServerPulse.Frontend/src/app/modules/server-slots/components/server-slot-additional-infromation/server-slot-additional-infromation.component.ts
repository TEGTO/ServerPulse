import { Component, Input, OnInit } from '@angular/core';
import { map, Observable, of } from 'rxjs';
import { ServerLoadStatisticsResponse } from '../../../shared';
import { ServerSlotDialogManager, ServerStatisticsService } from '../../index';

@Component({
  selector: 'server-slot-additional-info',
  templateUrl: './server-slot-additional-infromation.component.html',
  styleUrl: './server-slot-additional-infromation.component.scss'
})
export class ServerSlotAdditionalInfromationComponent implements OnInit {
  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  @Input({ required: true }) slotKey!: string;

  chartData$: Observable<Map<string, number>> = of(new Map<string, number>);

  constructor(
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.chartData$ = this.statisticsService.getLastServerLoadStatistics(this.slotKey).pipe(
      map(lastLoadStatistics => {
        return this.generateChartSeries(lastLoadStatistics?.statistics!);
      }),
    );
  }

  openDetailMenu(serializedEvent: string) {
    let test: string =
      "{\"Id\": \"someid\",\"Key\": \"ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff111\",\"CreationDateUTC\": \"2024-08-07T14:30:45.4656254Z\",\"Endpoint\": \"/api/v1/resource\",\"Method\": \"GET\",\"StatusCode\": 200,\"Duration\":\"00:00:00.1500000\",\"TimestampUTC\": \"2024-08-07T14:30:45.4656254Z\"}";
    this.dialogManager.openCustomEventDetails(test);
  }
  private generateChartSeries(statistics: ServerLoadStatisticsResponse | undefined): Map<string, number> {
    let data = new Map<string, number>();
    if (statistics && statistics.loadMethodStatistics) {
      let methodStatistics = statistics.loadMethodStatistics;
      data.set("GET", methodStatistics.getAmount);
      data.set("POST", methodStatistics.postAmount);
      data.set("PUT", methodStatistics.putAmount);
      data.set("PATCH", methodStatistics.patchAmount);
      data.set("DELETE", methodStatistics.deleteAmount);
    }
    return data;
  }
}