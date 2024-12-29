import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { AnalyzerEffects, serverCustomStatisticsReducer, serverLifecycleStatisticsReducer, serverLoadAmountStatisticsReducer, serverLoadStatisticsReducer } from '.';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    StoreModule.forFeature('lifecyclestatistics', serverLifecycleStatisticsReducer),
    StoreModule.forFeature('loadstatistics', serverLifecycleStatisticsReducer),
    StoreModule.forFeature('loadstatistics', serverLoadStatisticsReducer),
    StoreModule.forFeature('customstatistics', serverCustomStatisticsReducer),
    StoreModule.forFeature('loadamountstatistics', serverLoadAmountStatisticsReducer),
    EffectsModule.forFeature([AnalyzerEffects]),
  ]
})
export class AnalyzerModule { }
