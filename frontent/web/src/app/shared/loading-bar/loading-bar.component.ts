import { Component, inject } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingService } from '../../core/http/loading.service';

@Component({
  selector: 'app-loading-bar',
  standalone: true,
  imports: [NgIf, AsyncPipe, MatProgressBarModule],
  template: `<mat-progress-bar *ngIf="loading$ | async" mode="indeterminate"></mat-progress-bar>`,
  styleUrl: './loading-bar.component.scss',
})
export class LoadingBarComponent {
  private readonly loadingService = inject(LoadingService);
  readonly loading$ = this.loadingService.loading$;
}
