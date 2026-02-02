import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { AchievementsApi, AchievementDto } from '../../../core/api/achievements.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { AchievementDialogComponent } from './achievement-dialog.component';

@Component({
  selector: 'app-achievements',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, MatButtonModule, MatIconModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './achievements.component.html',
  styleUrl: './achievements.component.scss',
})
export class AchievementsComponent implements OnInit {
  private readonly api = inject(AchievementsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  achievements: AchievementDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.listAdmin(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.achievements = response.items;
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
    const ref = this.dialog.open(AchievementDialogComponent, { width: '480px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.create(result).subscribe(() => this.load());
    });
  }

  openEdit(achievement: AchievementDto): void {
    const ref = this.dialog.open(AchievementDialogComponent, {
      width: '480px',
      data: { achievement },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.update(achievement.id, result).subscribe(() => this.load());
    });
  }

  delete(achievement: AchievementDto): void {
    this.confirm
      .confirm({
        title: 'Delete achievement',
        message: `Delete ${achievement.title}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.delete(achievement.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
