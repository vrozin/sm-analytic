import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from "@angular/router";
import { Subscription } from 'rxjs';


import { UserService } from '../../../../shared/services/user.service';
import { LoginCredentials } from '../../../../shared/models/login-credentials'


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy
{
  private subscription: Subscription;

  newUser: boolean;
  userConfirmed: boolean = false;
  userId: string = '';
  userToken: string = '';
  confirmationMessage: string = '';
  errors: string = '';
  submitted: boolean = false;
  credentials: LoginCredentials = { Email: '', Password: '' };
  isBusy: boolean;

  constructor(private userService: UserService, private router: Router, private activatedRoute: ActivatedRoute) { }

  ngOnInit() {

    // subscribe to router event
    this.subscription = this.activatedRoute.queryParams.subscribe(
      (param: any) =>
      {
        this.newUser = param['newUser'];
        this.credentials.Email = param['Email'];
        this.userConfirmed = param['userConfirmed'];
        this.userId = param['userId'];
        this.userToken = param['confirmationToken'];
      });

    if (this.userConfirmed)
    {
      this.confirmAccount(this.userId, this.userToken);
    }
  }

  ngOnDestroy()
  {
    this.subscription.unsubscribe();
  }

  login({ value, valid }: { value: LoginCredentials, valid: boolean })
  {
    this.submitted = true;
    this.errors = '';
    this.isBusy = true;

    if (valid)
    {
      this.userService.login(value.Email, value.Password)
        .finally(() => this.isBusy = false)
        .subscribe(
        result =>
        {
          if (result)
          {
              this.router.navigate(['dashboard']);
          }
        },
        error => this.errors = error);
    }
  }

  resetPassword()
  {
    this.router.navigate(['auth/passwordReset']);
  }

  confirmAccount(userId: string, userToken: string) {
    this.userService.confirmAccount(userId, userToken)
      .subscribe(
        result => {
          this.confirmationMessage = result.message;
      });
  }

}
