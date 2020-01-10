import { Component } from "@angular/core";
import { ZipInfoDataService } from "../services/zip-info-data.service";
import { ZipInfoDto } from "../dto/zip-info-dto";
import {
  FormGroup,
  FormBuilder,
  FormControl,
  Validators
} from "@angular/forms";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.scss"]
})
export class HomeComponent {
  public currentZipInfoDto: ZipInfoDto;
  form: FormGroup;

  constructor(private zipInfoDataService: ZipInfoDataService, fb: FormBuilder) {
    this.form = fb.group({
      zipCode: new FormControl("", Validators.required)
    });
  }

  search() {
    if (this.form.invalid) {
      return;
    }

    const zipCode = this.form.get("zipCode").value;

    this.zipInfoDataService.getZipInfo(zipCode).subscribe(
      result => {
        this.currentZipInfoDto = result;
      },
      error => {
        console.log(error);
      }
    );
  }

  getText(value: ZipInfoDto) {
    return `At the location ${value.cityName}, the temperature is ${value.temperature}, and the timezone is ${value.timeZone}`;
  }
}
