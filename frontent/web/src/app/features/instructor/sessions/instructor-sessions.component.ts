import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { InstructorApi } from '../../../core/api/instructor.api';
import { SessionDto } from '../../../core/api/groups.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';

@Component({
  selector: 'app-instructor-sessions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgFor,
    NgIf,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    PaginationComponent,
    SkeletonCardsComponent,
  ],
  templateUrl: './instructor-sessions.component.html',
  styleUrl: './instructor-sessions.component.scss',
})
export class InstructorSessionsComponent implements OnInit {
  private readonly api = inject(InstructorApi);

  sessions: SessionDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  from = '';
  to = '';
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api
      .listMySessions(this.page, this.pageSize, this.from || undefined, this.to || undefined)
      .subscribe({
        next: (response) => {
          this.sessions = response.items;
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
