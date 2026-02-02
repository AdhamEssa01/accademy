import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { JsonPipe, NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { DashboardsApi } from '../../../core/api/dashboards.api';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgIf, NgFor, JsonPipe, MatCardModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent implements OnInit {
  isLoading = true;
  kpis: Array<{ label: string; value: string }> = [];
  lists: Array<{ title: string; items: Array<Record<string, unknown>> }> = [];

  constructor(private readonly api: DashboardsApi) {}

  ngOnInit(): void {
    this.api.getAdmin().subscribe({
      next: (data) => {
        this.kpis = this.buildKpis(data);
        this.lists = this.buildLists(data);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  private buildKpis(data: Record<string, unknown>) {
    return Object.entries(data)
      .filter(([, value]) => typeof value === 'number' || typeof value === 'string')
      .map(([key, value]) => ({
        label: this.formatKey(key),
        value: String(value),
      }));
  }

  private buildLists(data: Record<string, unknown>) {
    return Object.entries(data)
      .filter(([, value]) => Array.isArray(value))
      .map(([key, value]) => ({
        title: this.formatKey(key),
        items: (value as Array<Record<string, unknown>>).slice(0, 5),
      }));
  }

  private formatKey(key: string): string {
    return key
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/_/g, ' ')
      .replace(/\b\w/g, (char) => char.toUpperCase());
  }
}
