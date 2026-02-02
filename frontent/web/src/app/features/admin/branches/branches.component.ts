import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { BranchesApi, BranchDto } from '../../../core/api/branches.api';
import { BranchDialogComponent } from './branch-dialog.component';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { PagedResponse } from '../../../shared/pagination/paging.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-branches',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, MatButtonModule, MatIconModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './branches.component.html',
  styleUrl: './branches.component.scss',
})
export class BranchesComponent implements OnInit {
  private readonly api = inject(BranchesApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  branches: BranchDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.list(this.page, this.pageSize).subscribe({
      next: (response: PagedResponse<BranchDto>) => {
        this.branches = response.items;
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
    const ref = this.dialog.open(BranchDialogComponent, { width: '420px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.create(result).subscribe(() => this.load());
    });
  }

  openEdit(branch: BranchDto): void {
    const ref = this.dialog.open(BranchDialogComponent, {
      width: '420px',
      data: { branch },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.update(branch.id, result).subscribe(() => this.load());
    });
  }

  delete(branch: BranchDto): void {
    this.confirm
      .confirm({
        title: 'Delete branch',
        message: `Delete ${branch.name}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.delete(branch.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
