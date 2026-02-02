import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { ParentApi, AssignmentDto } from '../../../core/api/parent.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';

@Component({
  selector: 'app-parent-assignments',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './parent-assignments.component.html',
  styleUrl: './parent-assignments.component.scss',
})
export class ParentAssignmentsComponent implements OnInit {
  private readonly api = inject(ParentApi);

  assignments: AssignmentDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.getAssignments(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.assignments = response.items;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.total = response.total;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
