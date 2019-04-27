import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from "@angular/router";
import { DashboardUser } from '../../../../shared/models/dashboard-user';
import { DashboardService } from '../../dashboard.service';
import { Subscription } from 'rxjs/Subscription';
import { UserService } from '../../../../shared/services/user.service';
import { TwitterDataService } from '../../../../shared/services/twitter-data.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  appTitle: string = "Social Media Analytics";
  dashboardUser: DashboardUser;
  dashboardUserFN: string = "";
  dashboardUserLN: string = "";
  dashboardUserIsAdmin: boolean = false;

  options: Object[];

  private dashboardUserSubscr: Subscription = null;
  
  constructor(private router: Router,
    private dashboardService: DashboardService,
    private userService: UserService,
    private twitterService: TwitterDataService
  ) { }

  ngOnInit()
  {
    this.options = [
      //{ 'title': 'Profile', 'path': 'dashboard/profile' },
      { 'title': 'FAQ', 'path': 'dashboard/faq' },
      { 'title': 'Help', 'path': 'dashboard/help' },
    ];

    this.dashboardUserSubscr = this.dashboardService.getAuthDetails().subscribe((dashboardUser: DashboardUser) =>
    {
      this.dashboardUser        = dashboardUser;
      this.dashboardUserFN      = dashboardUser.firstName;
      this.dashboardUserLN      = dashboardUser.lastName;
      this.dashboardUserIsAdmin = dashboardUser.isAdmin;

      if (this.dashboardUserIsAdmin)
      {
        this.options.push(new Object({ 'title': 'Admin Panel', 'path': 'dashboard/pending-users' }));
        this.options.push(new Object({ 'title': 'Broadcast', 'path': 'dashboard/broadcast-message' }));
      }

      this.options.push(new Object({ 'title': 'Logout', 'path': 'landing' }));
    },
      error => { });
  }

  ngOnDestroy()
  {
    this.dashboardUserSubscr.unsubscribe();
  }

  faq() {
    this.router.navigate(['faq']);
  }

  gotoMenuPage(path) {
    if (path == 'landing') {
      this.twitterService.clearSession();
      this.userService.logout();
      localStorage.clear();
    }

    this.router.navigate([path]);
  }
}
