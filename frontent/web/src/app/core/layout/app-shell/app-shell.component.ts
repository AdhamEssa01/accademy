import { Component, inject, OnInit } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../auth/auth.service';
import { Roles } from '../../auth/roles';
import { UserInfo } from '../../auth/auth.models';
import { SessionBannerComponent } from '../../../shared/session-banner/session-banner.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    AsyncPipe,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule,
    MatDividerModule,
    SessionBannerComponent,
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly user$ = this.auth.user$;

  private readonly adminMenu = [
    { label: 'Academy', icon: 'school', route: '/app/admin/academy' },
    { label: 'Branches', icon: 'apartment', route: '/app/admin/branches' },
    { label: 'Programs', icon: 'menu_book', route: '/app/admin/programs' },
    { label: 'Courses', icon: 'library_books', route: '/app/admin/courses' },
    { label: 'Levels', icon: 'layers', route: '/app/admin/levels' },
    { label: 'Groups', icon: 'groups', route: '/app/admin/groups' },
    { label: 'Sessions', icon: 'event', route: '/app/admin/sessions' },
    { label: 'Students', icon: 'face', route: '/app/admin/students' },
    { label: 'Attendance', icon: 'event_available', route: '/app/admin/attendance' },
    { label: 'Homework', icon: 'assignment', route: '/app/admin/attendance' },
    { label: 'Announcements', icon: 'campaign', route: '/app/admin/attendance' },
    { label: 'Achievements', icon: 'emoji_events', route: '/app/admin/achievements' },
  ];

  private readonly instructorMenu = [
    { label: 'My Groups', icon: 'groups', route: '/app/instructor/groups' },
    { label: 'My Sessions', icon: 'event', route: '/app/instructor/sessions' },
    { label: 'Attendance', icon: 'check_circle', route: '/app/instructor/attendance' },
    { label: 'Homework', icon: 'assignment', route: '/app/instructor/assignments' },
  ];

  private readonly parentMenu = [
    { label: 'Children', icon: 'child_care', route: '/app/parent/children' },
    { label: 'Attendance', icon: 'fact_check', route: '/app/parent/attendance' },
    { label: 'Homework', icon: 'home_work', route: '/app/parent/assignments' },
    { label: 'Announcements', icon: 'notifications', route: '/app/parent/announcements' },
  ];

  ngOnInit(): void {
    if (this.auth.isAuthenticated() && !this.auth.getUser()) {
      this.auth.me().subscribe({
        error: () => {
          this.auth.clearSession();
          void this.router.navigateByUrl('/login');
        },
      });
    }
  }

  getMenuItems(user: UserInfo | null) {
    const roles = user?.roles ?? [];
    const items = [{ label: 'Overview', icon: 'dashboard', route: '/app' }];

    if (roles.includes(Roles.Admin)) {
      items.push(...this.adminMenu);
    }
    if (roles.includes(Roles.Instructor)) {
      items.push(...this.instructorMenu);
    }
    if (roles.includes(Roles.Parent)) {
      items.push(...this.parentMenu);
    }

    return items;
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => {
        void this.router.navigateByUrl('/login');
      },
    });
  }
}
