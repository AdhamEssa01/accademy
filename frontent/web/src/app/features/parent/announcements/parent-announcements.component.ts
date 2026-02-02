import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { ParentApi, AnnouncementDto } from '../../../core/api/parent.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';

@Component({
  selector: 'app-parent-announcements',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './parent-announcements.component.html',
  styleUrl: './parent-announcements.component.scss',
})
export class ParentAnnouncementsComponent implements OnInit {
  private readonly api = inject(ParentApi);

  announcements: AnnouncementDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.getAnnouncements(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.announcements = response.items;
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
