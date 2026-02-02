import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { InstructorApi } from '../../../core/api/instructor.api';
import { GroupDto } from '../../../core/api/groups.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';

@Component({
  selector: 'app-instructor-groups',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './instructor-groups.component.html',
  styleUrl: './instructor-groups.component.scss',
})
export class InstructorGroupsComponent implements OnInit {
  private readonly api = inject(InstructorApi);

  groups: GroupDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.listMyGroups(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.groups = response.items;
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
