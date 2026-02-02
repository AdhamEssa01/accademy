import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { CmsApi, CmsSection } from '../../../core/api/cms.api';
import { AchievementsApi, AchievementDto } from '../../../core/api/achievements.api';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgIf,
    NgFor,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
  ],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
})
export class LandingPageComponent implements OnInit {
  cmsSections: Array<{ type: string; data: Record<string, unknown> }> = [];
  achievements: AchievementDto[] = [];
  cmsLoaded = false;
  selectedAchievementTag = 'All';

  constructor(
    private readonly cmsApi: CmsApi,
    private readonly achievementsApi: AchievementsApi
  ) {}

  ngOnInit(): void {
    this.loadCms();
    this.loadAchievements();
  }

  private loadCms(): void {
    this.cmsApi.getPage('landing').subscribe((page) => {
      const sections = page?.sections?.length ? page.sections : null;
      if (sections) {
        this.cmsSections = sections.map((section) => ({
          type: section.type,
          data: this.parseSectionData(section),
        }));
      }
      this.cmsLoaded = true;
      if (!sections) {
        this.cmsApi.getPage('home').subscribe((homePage) => {
          if (homePage?.sections?.length) {
            this.cmsSections = homePage.sections.map((section) => ({
              type: section.type,
              data: this.parseSectionData(section),
            }));
          }
        });
      }
    });
  }

  private loadAchievements(): void {
    this.achievementsApi.listPublic(1, 6).subscribe((response) => {
      this.achievements = response.items;
    });
  }

  get achievementTags(): string[] {
    const tags = this.achievements
      .map((item) => item.tag)
      .filter((tag): tag is string => !!tag);
    return ['All', ...Array.from(new Set(tags))];
  }

  get filteredAchievements(): AchievementDto[] {
    if (this.selectedAchievementTag === 'All') {
      return this.achievements;
    }
    return this.achievements.filter((item) => item.tag === this.selectedAchievementTag);
  }

  getText(data: Record<string, unknown>, key: string, fallback = ''): string {
    const value = data?.[key];
    return typeof value === 'string' && value.trim().length > 0 ? value : fallback;
  }

  getLink(data: Record<string, unknown>, key: string, fallback: string): string {
    const value = data?.[key];
    return typeof value === 'string' && value.trim().length > 0 ? value : fallback;
  }

  getArray(data: Record<string, unknown>, key: string): any[] {
    const value = data?.[key];
    return Array.isArray(value) ? value : [];
  }

  private parseSectionData(section: CmsSection): Record<string, unknown> {
    if (section.jsonContent) {
      try {
        return JSON.parse(section.jsonContent) as Record<string, unknown>;
      } catch {
        return {};
      }
    }
    if (section.content && typeof section.content === 'object') {
      return section.content as Record<string, unknown>;
    }
    return {};
  }
}
