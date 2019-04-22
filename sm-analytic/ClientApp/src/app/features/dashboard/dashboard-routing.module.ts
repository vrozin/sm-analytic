import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { OverviewComponent } from './pages/overview/overview.component';
import { SentimentComponent } from './pages/sentiment/sentiment.component';
import { FollowerComponent } from './pages/follower/follower.component';
import { TrendComponent } from './pages/trend/trend.component';
import { DashboardComponent } from './dashboard.component';
import { FaqComponent } from './pages/faq/faq.component';
import { HelpComponent } from './pages/help/help.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { PendingUsersComponent } from './pages/pending-users/pending-users.component';
import { AuthGuard } from '../auth/auth.guard';
import { BroadcastMessageComponent } from './pages/broadcast-message/broadcast-message.component';

const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent, canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: OverviewComponent
      },
      {
        path: 'trend',
        component: TrendComponent
      },
      {
        path: 'sentiment',
        component: SentimentComponent
      },
      {
        path: 'follower',
        component: FollowerComponent
      },
      {
        path: 'faq',
        component: FaqComponent
      },
      {
        path: 'help',
        component: HelpComponent
      },
      {
        path: 'profile',
        component: ProfileComponent
      },
      {
        path: 'pending-users',
        component: PendingUsersComponent
      },
      {
        path: 'broadcast-message',
        component: BroadcastMessageComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
