import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog } from '@angular/material/dialog';
import { GroupsApi, SessionDto } from '../../../core/api/groups.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { SessionDialogComponent } from './session-dialog.component';

@Component({
  selector: 'app-sessions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgFor,
    NgIf,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    PaginationComponent,
    SkeletonCardsComponent,
  ],
  templateUrl: './sessions.component.html',
  styleUrl: './sessions.component.scss',
})
export class SessionsComponent implements OnInit {
  private readonly api = inject(GroupsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  sessions: SessionDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  from?: string;
  to?: string;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.listSessions(this.page, this.pageSize, this.from, this.to).subscribe({
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

  openCreate(): void {
    const ref = this.dialog.open(SessionDialogComponent, { width: '520px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.createSession(result).subscribe(() => this.load());
    });
  }

  openEdit(session: SessionDto): void {
    const ref = this.dialog.open(SessionDialogComponent, {
      width: '520px',
      data: { session },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.updateSession(session.id, result).subscribe(() => this.load());
    });
  }

  delete(session: SessionDto): void {
    this.confirm
      .confirm({
        title: 'Delete session',
        message: `Delete session starting ${session.startsAtUtc}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.deleteSession(session.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
