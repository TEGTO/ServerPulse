import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { BehaviorSubject, debounceTime, Subject, takeUntil } from 'rxjs';
import { ServerSlot, updateServerSlot, UpdateSlotRequest } from '../..';

@Component({
  selector: 'app-server-slot-name-changer',
  templateUrl: './server-slot-name-changer.component.html',
  styleUrl: './server-slot-name-changer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotNameChangerComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input({ required: true }) serverSlot!: ServerSlot;
  @Input({ required: true }) inputIsEditable$!: BehaviorSubject<boolean>;
  @ViewChild('textSizer') textSizer!: ElementRef;
  @ViewChild('slotInput') slotInputElement!: ElementRef<HTMLInputElement>;

  inputControl = new FormControl('');

  inputWidth$ = new BehaviorSubject<number>(120);
  private readonly destroy$ = new Subject<void>();

  constructor(private readonly store: Store) { }

  ngOnInit(): void {
    this.inputControl.setValue(this.serverSlot.name);

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

  onBlur(): void {
    this.validateAndSaveInput();
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

  private updateServerSlot(): void {
    const request: UpdateSlotRequest = {
      id: this.serverSlot.id,
      name: this.inputControl.value ?? '',
    };

    this.store.dispatch(updateServerSlot({ req: request }));
  }
}