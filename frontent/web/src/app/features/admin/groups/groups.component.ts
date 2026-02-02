import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { GroupsApi, GroupDto } from '../../../core/api/groups.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { GroupDialogComponent } from './group-dialog.component';

@Component({
  selector: 'app-groups',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, MatButtonModule, MatIconModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './groups.component.html',
  styleUrl: './groups.component.scss',
})
export class GroupsComponent implements OnInit {
  private readonly api = inject(GroupsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

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
    this.api.listGroups(this.page, this.pageSize).subscribe({
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

  openCreate(): void {
    const ref = this.dialog.open(GroupDialogComponent, { width: '480px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.createGroup(result).subscribe(() => this.load());
    });
  }

  openEdit(group: GroupDto): void {
    const ref = this.dialog.open(GroupDialogComponent, {
      width: '480px',
      data: { group },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.updateGroup(group.id, result).subscribe(() => this.load());
    });
  }

  delete(group: GroupDto): void {
    this.confirm
      .confirm({
        title: 'Delete group',
        message: `Delete ${group.name}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.deleteGroup(group.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
