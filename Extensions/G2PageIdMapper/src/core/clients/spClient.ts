import * as sprequest from 'sp-request';

export class SPClient {
    private client: sprequest.ISPRequest;

    public get Client():sprequest.ISPRequest {
        return this.client;
    }
    constructor(options : sprequest.IUserCredentials){
        this.client = sprequest.create(options);
    }

}