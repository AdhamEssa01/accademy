import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { ProgramsApi, ProgramDto } from '../../../core/api/programs.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { ProgramDialogComponent } from './program-dialog.component';

@Component({
  selector: 'app-programs',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, MatCardModule, MatButtonModule, MatIconModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './programs.component.html',
  styleUrl: './programs.component.scss',
})
export class ProgramsComponent implements OnInit {
  private readonly api = inject(ProgramsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  programs: ProgramDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.listPrograms(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.programs = response.items;
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
    const ref = this.dialog.open(ProgramDialogComponent, { width: '440px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.createProgram(result).subscribe(() => this.load());
    });
  }

  openEdit(program: ProgramDto): void {
    const ref = this.dialog.open(ProgramDialogComponent, {
      width: '440px',
      data: { program },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.updateProgram(program.id, result).subscribe(() => this.load());
    });
  }

  delete(program: ProgramDto): void {
    this.confirm
      .confirm({
        title: 'Delete program',
        message: `Delete ${program.name}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.deleteProgram(program.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
