import { Component, OnInit } from '@angular/core';
import { UserService } from '../../../../shared/services/user.service';
import { EmailSubjectMessage } from 'app/shared/models/email-message';

@Component({
  selector: 'app-broadcast-message',
  templateUrl: './broadcast-message.component.html',
  styleUrls: ['./broadcast-message.component.scss']
})
export class BroadcastMessageComponent implements OnInit {

  errors: string = '';
  errorsBool: boolean = false;
  submitted: boolean = false;
  isBusy: boolean = false;
  messageBack: boolean = false;

  constructor(
    private userService: UserService) { }

  ngOnInit() { }

  sendBroadcastEmail({ value, valid }: { value: EmailSubjectMessage, valid: boolean }) {
    this.submitted = true;
    this.errors = '';
    this.errorsBool = false;
    this.isBusy = true;
    this.messageBack = false;

    if (value.Subject.trim().length == 0) {
      this.errors = "The subject cannot be empty of have only spaces";
      this.errorsBool = true;
    }
    else
    if (value.Message.trim().length == 0) {
      this.errors = "The message cannot be empty of have only spaces";
      this.errorsBool = true;
    }
    else
      if (valid) {
        console.log("Sending broadcast! Sub: " + value.Subject.trim() + "  Msg: " + value.Message.trim());
        this.userService.sendBroadcastEmail(value.Subject.trim(), value.Message.trim())
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
