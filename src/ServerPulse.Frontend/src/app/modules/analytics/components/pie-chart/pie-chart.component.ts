import { AfterViewInit, ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ApexChart, ApexLegend, ApexNonAxisChartSeries } from 'ng-apexcharts';
import { Observable, Subject, takeUntil } from 'rxjs';

export type PieChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: any;
  legend: ApexLegend;
};

@Component({
  selector: 'pie-chart',
  templateUrl: './pie-chart.component.html',
  styleUrl: './pie-chart.component.scss'
})
export class PieChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input({ required: true }) data$!: Observable<Map<string, number>>;
  chartOptions!: Partial<PieChartOptions>;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.initChartOptions();
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewInit(): void {
    this.data$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.updateChartData(data));
  }

  private initChartOptions() {
    this.chartOptions = {
      series: [44, 55, 13, 43, 22],
      chart: {
        width: 380,
        type: "pie",
        selection: {
          enabled: false
        },
        animations: {
          enabled: false
        }
      },
      labels: ["GET", "POST", "PUT", "PATCH", "DELETE"],
      legend: {
        position: 'bottom',
      },
    };
  }

  private updateChartData(data: Map<string, number>) {
    if (this.chartOptions != null && this.chartOptions.series != null) {
      let labels: string[] = [];
      let series: number[] = [];
      data.forEach((v, k) => {
        labels.push(k);
        series.push(v);
      });

      this.chartOptions.series = series;
      this.chartOptions.labels = labels;

      this.cdr.detectChanges();
    }
  }
}