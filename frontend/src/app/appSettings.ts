import { environment } from '../environments/environment';

export class AppSettings {
    /* For debugging the front end (i.e. running it from the dev machine using ng serve)
      set the DEV_API_ENDPOINT to the root address of your quest's IP on your network */
    private static DEV_API_ENDPOINT = 'http://192.168.0.105:50000';
    /* set this value to override what the server returns as the websocket address.  Useful if using
      an emulator and forward the ports through ADB */
    private static DEV_WS_ENDPOINT = null; //'ws://localhost:51001';

    public static API_ENDPOINT = environment.production ? '' : AppSettings.DEV_API_ENDPOINT;

    public static WS_ENDPOINT_OVERRIDE = environment.production ? null : AppSettings.DEV_WS_ENDPOINT;

    // public static API_ENDPOINT= 'http://localhost:50000';

    //public static WS_ENDPOINT_OVERRIDE = null;
}
