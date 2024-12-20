import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { BehaviorSubject, debounceTime, Subject, takeUntil } from 'rxjs';
import { deleteServerSlot, ServerSlot, showSlotInfo, showSlotKey, updateServerSlot, UpdateServerSlotRequest } from '../../../server-slot-shared';

export enum ServerStatus {
  Online = 'Online',
  Offline = 'Offline',
  NoData = 'No Data'
}

@Component({
  selector: 'app-server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input({ required: true }) serverSlot!: ServerSlot;
  @ViewChild('textSizer') textSizer!: ElementRef;
  @ViewChild('slotInput') slotInputElement!: ElementRef<HTMLInputElement>;

  inputControl = new FormControl('');
  inputIsEditable$ = new BehaviorSubject<boolean>(false);
  inputWidth$ = new BehaviorSubject<number>(120);

  serverStatus$ = new BehaviorSubject<ServerStatus>(ServerStatus.NoData);

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly store: Store) { }

  ngOnInit(): void {
    this.inputControl.setValue(this.serverSlot.name);

    // this.serverStatisticsService.getLastServerStatistics(this.serverSlot.slotKey).pipe(
    //   tap(message => this.handleStatisticsMessage(message)),
    //   takeUntil(this.destroy$)
    // ).subscribe();

    this.inputControl.valueChanges.pipe(
      debounceTime(300),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  ngAfterViewInit(): void {
    this.adjustInputWidth();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // private handleStatisticsMessage(message: { key: string; statistics: ServerStatisticsResponse; } | null): void {
  //   if (!message || message.key !== this.serverSlot.slotKey) {
  //     return;
  //   }
  //   this.updateServerStatus(message.statistics);
  // }

  // private updateServerStatus(statistics: ServerStatisticsResponse): void {
  //   if (statistics.dataExists) {
  //     this.serverStatus$.next(statistics.isAlive ? ServerStatus.Online : ServerStatus.Offline);
  //   } else {
  //     this.serverStatus$.next(ServerStatus.NoData);
  //   }
  // }

  onBlur(): void {
    this.validateAndSaveInput();
  }

  showSlotInfo(): void {
    this.store.dispatch(showSlotInfo({ id: this.serverSlot.id }));
  }

  showKey(): void {
    this.store.dispatch(showSlotKey({ slot: this.serverSlot }));
  }

  private adjustInputWidth(): void {
    if (this.textSizer) {
      const sizer = this.textSizer.nativeElement;
      const newWidth = Math.min(sizer.scrollWidth + 1, 400);
      setTimeout(() => {
        this.inputWidth$.next(newWidth);
      });
    }
  }

  private validateAndSaveInput(): void {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlot();
    this.adjustInputWidth();
  }

  private checkEmptyInput(): void {
    if (!this.inputControl.value?.trim()) {
      this.inputControl.setValue('New slot');
    }
  }

  makeInputEditable(): void {
    this.inputIsEditable$.next(true);
    setTimeout(() => {
      this.slotInputElement.nativeElement.focus();
    });
  }

  private makeInputNonEditable(): void {
    this.inputIsEditable$.next(false);
  }

  deleteSlot(): void {
    this.store.dispatch(deleteServerSlot({ id: this.serverSlot.id }));
  }

  private updateServerSlot(): void {
    const request: UpdateServerSlotRequest = {
      id: this.serverSlot.id,
      name: this.inputControl.value ?? '',
    };

    this.store.dispatch(updateServerSlot({ req: request }));
  }
}
