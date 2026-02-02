import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ParentApi, ParentChildDto } from '../../../core/api/parent.api';

@Component({
  selector: 'app-parent-children',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, SkeletonCardsComponent],
  templateUrl: './parent-children.component.html',
  styleUrl: './parent-children.component.scss',
})
export class ParentChildrenComponent implements OnInit {
  private readonly api = inject(ParentApi);

  children: ParentChildDto[] = [];
  isLoading = false;

  ngOnInit(): void {
    this.isLoading = true;
    this.api.getChildren().subscribe({
      next: (children) => {
        this.children = children;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }
}
