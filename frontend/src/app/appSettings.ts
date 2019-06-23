import { environment } from '../environments/environment';

export class AppSettings {
   /* For debugging the front end (i.e. running it from the dev machine using ng serve)
      set the DEV_API_ENDPOINT to the root address of your quest's IP on your network */
   private static DEV_API_ENDPOINT='';//'http://192.168.1.250:50000';

   public static API_ENDPOINT= environment.production?'':AppSettings.DEV_API_ENDPOINT;
}