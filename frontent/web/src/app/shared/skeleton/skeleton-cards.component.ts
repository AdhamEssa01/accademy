import { Component, Input } from '@angular/core';
import { NgFor } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-skeleton-cards',
  standalone: true,
  imports: [NgFor, MatCardModule],
  templateUrl: './skeleton-cards.component.html',
  styleUrl: './skeleton-cards.component.scss',
})
export class SkeletonCardsComponent {
  @Input() count = 4;
  skeletons = Array.from({ length: 4 });

  ngOnChanges(): void {
    this.skeletons = Array.from({ length: this.count });
  }
}
