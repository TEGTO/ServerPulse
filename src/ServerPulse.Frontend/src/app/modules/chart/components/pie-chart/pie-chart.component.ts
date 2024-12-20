import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subject, takeUntil } from 'rxjs';
import { PieChartOptions } from '../..';

@Component({
  selector: 'app-pie-chart',
  templateUrl: './pie-chart.component.html',
  styleUrl: './pie-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PieChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input({ required: true }) data$!: Observable<Map<string, number>>;
  chartOptions!: Partial<PieChartOptions>;

  private readonly destroy$ = new Subject<void>();

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
    if (this.chartOptions?.series != null) {
      const labels: string[] = [];
      const series: number[] = [];
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
