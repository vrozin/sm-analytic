import { Component, OnInit } from '@angular/core';
import { DashboardUser } from 'app/shared/models/dashboard-user';

import { UserService } from 'app/shared/services/user.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-pending-users',
  templateUrl: './pending-users.component.html',
  styleUrls: ['./pending-users.component.scss']
})
export class PendingUsersComponent implements OnInit {

  userData: DashboardUser[] = [];
  accountActioned: boolean[] = [];
  accountConfirmedByAdmin: boolean[] = [];
  actionMessage: string[] = [];

  private userDataSubscr: Subscription = null;

  constructor(private userService: UserService) { }

  ngOnInit() {
    this.getTableData();
  }

  getTableData() {
    if (this.userDataSubscr != null) {
      this.userDataSubscr.unsubscribe();
    }

    while (this.userData.length > 0)
    {
      this.userData.pop();
    }

    this.userData = [];
    this.accountActioned = [];
    this.accountConfirmedByAdmin = [];
    this.actionMessage = [];

    this.userDataSubscr = this.userService.getAllUsers().subscribe((result: DashboardUser[]) => {
      result.forEach((user, index) => {
        this.userData.push(user);
        this.accountActioned.push(false);
        this.accountConfirmedByAdmin.push(user.emailConfirmedByAdmin);
        this.actionMessage.push('');
      });
    });
  }

  ngOnDestroy() {
    this.userDataSubscr.unsubscribe();
  }

  callForConfirmation(index: number) {
    this.accountActioned[index] = true;

    this.userService.confirmAccountAdmin(this.userData[index].email)
      .subscribe(result => {
        this.actionMessage = result.message;
        this.getTableData();
      });
  }

  callForDeletion(index: number) {
    this.accountActioned[index] = true;

    this.userService.deleteAccountAdmin(this.userData[index].email)
      .subscribe(result => {
        this.actionMessage = result.message;
        this.getTableData();
      });

    
  }

}
