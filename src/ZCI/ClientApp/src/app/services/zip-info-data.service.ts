import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { catchError, share } from "rxjs/operators";
import { HttpClient, HttpParams } from "@angular/common/http";
import { BaseDataService } from "./base-data.service";
import { ZipInfoDto } from "../dto/zip-info-dto";

@Injectable({
  providedIn: "root"
})
export class ZipInfoDataService extends BaseDataService {
  constructor(http: HttpClient) {
    super(http);
  }

  getZipInfo(zipCode: string): Observable<ZipInfoDto> {
    return this.http
      .get<ZipInfoDto>(`api/zipinfo/${zipCode}`)
      .pipe(catchError(this.handleError), share());
  }
}
