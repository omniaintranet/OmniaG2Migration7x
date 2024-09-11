import { SPClient } from "../core";
import { ImportPagesReport, RefinedImportPagesItem, SpPageResult, OmniaPageListItem } from "../models";

export class LegacyExportPageReportRefiner {
    private Spsite: string;
    private spClient: SPClient;
    private listId: string;
    constructor(sites: string, client: SPClient, pagelistId: string) {
        this.Spsite = sites;
        this.spClient = client;
        this.listId = pagelistId;
    }

    public refineReport(report: ImportPagesReport) {
        return this.retrieveAllSpPages().then((data)=>{
            let refinedList: Array<RefinedImportPagesItem> = [];
            report.SucceedItems.forEach((item) => {
                let refinedItem: RefinedImportPagesItem = {...{G2PhysicalPageUniqueId: "Not found"},...item};
                let mapped = data.get(refinedItem.PageId);
                if(!!mapped)
                refinedItem.G2PhysicalPageUniqueId = mapped?.GUID;
                refinedList.push(refinedItem);
            })
            report.SucceedItems = refinedList;
            return report
        });
    }

    private retrieveAllSpPages() {
        let requestUrl = `${this.Spsite}/_api/Web/Lists(guid'${this.listId}')/items?$select=Id,OmniaPageId,Title,GUID`;
        let result = new Map<number,OmniaPageListItem>();
        let gatherPages = (url: string,result:Map<number,OmniaPageListItem>, resolve: (data:Map<number,OmniaPageListItem>) => any,reject: (error: string)=> any) => {
            this.spClient.Client.get(url).then((response) => {
                if(response.statusCode === 200){
                    let rspResult: SpPageResult<OmniaPageListItem> = response.body.d;
                    rspResult.results.map((listItem) =>{
                        if(!!listItem.OmniaPageId){
                            result.set(listItem.OmniaPageId,listItem);
                        }
                    });
                    if(!!rspResult.__next){
                        gatherPages(rspResult.__next,result,resolve,reject);
                    }else{
                        resolve(result);
                    }
                }else{
                    reject(JSON.stringify(response.body));
                }
            });
        };
        return new Promise((resolve: (list: Map<number,OmniaPageListItem>) => any) => {
            gatherPages(requestUrl,result,(data)=>{
                resolve(data);
            },(error)=>{
                console.log(error);
            });
        });
    }
}