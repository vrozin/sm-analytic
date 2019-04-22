import { Component, OnInit } from '@angular/core';
import { UserService } from '../../../../shared/services/user.service';
import { EmailMessage } from 'app/shared/models/email-message';
import { Router } from '@angular/router';
import { DashboardService } from '../../dashboard.service';


@Component({
  selector: 'app-help',
  templateUrl: './help.component.html',
  styleUrls: ['./help.component.scss']
})
export class HelpComponent implements OnInit {

  errors: string = '';
  errorsBool: boolean = false;
  submitted: boolean = false;
  isBusy: boolean = false;
  messageBack: boolean = false;

  constructor(
    private userService: UserService) { }

  ngOnInit() {}

  sendHelpEmail({ value, valid }: { value: EmailMessage, valid: boolean }) {
    this.submitted = true;
    this.errors = '';
    this.errorsBool = false;
    this.isBusy = true;
    this.messageBack = false;

    if (value.Message.trim().length == 0) {
      this.errors = "The message cannot be empty of have only spaces";
      this.errorsBool = true;
    }
    else
      if (valid) {
        this.userService.sendEmail(/*process.env.AdminEmail*/'smanalyticjmv@gmail.com', value.Message.trim())
        .subscribe(result => {
          if (result.result != 0) {
            this.messageBack = true;
          }

        },
        error => console.log(error));

    }
    this.isBusy = false;

  }

}
