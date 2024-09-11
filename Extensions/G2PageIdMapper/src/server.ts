
import * as env from "dotenv";
import * as fs from "fs";
import { SPClient,Constants } from './core';
import { ImportPagesReport } from "./models/ImportPagesReport";
import { LegacyExportPageReportRefiner } from "./services";
import { rejects } from "assert";

function init() {
  env.config();
  main().then(()=>{console.log("File Exported")});
}

async function main(): Promise<void> {
  let OmniaVersion = Number.parseFloat(process.env[Constants.EnvironmentVariables.OmniaVersion] || "5");
  let Reportpath = process.env[Constants.EnvironmentVariables.ReportPath] || "../report.json";
  let data: ImportPagesReport;
  try{
    data = JSON.parse(fs.readFileSync(Reportpath, { encoding: "utf8" }));
  }catch(Err){
    console.log("Cannot find report");
    return;
  }
  let site = process.env[Constants.EnvironmentVariables.ExportSite];
  let listId = process.env[Constants.EnvironmentVariables.PageListId] || "";
  try {
    if (!!site) {
      let client = getSPClient();
      let collector = new LegacyExportPageReportRefiner(site, client, listId);
      await collector.refineReport(data).then((report) =>{
        fs.writeFileSync(Reportpath,JSON.stringify(report),{ encoding: "utf8"});
      });
    }
  }catch(err){
    console.log(err);
  }
}

function getSPClient(): SPClient {

  let username = process.env[Constants.EnvironmentVariables.Username];
  let password = process.env[Constants.EnvironmentVariables.Password];
  let isOnline = (process.env["SP_isOnline"] ? process.env["SP_isOnline"].toLowerCase() === "true" : true)
  if (!!username && !!password) {
    let client = new SPClient({
      username: username,
      password: password,
      online: isOnline
    })
    return client;
  } else {
    throw new Error("Missing Authentication Params in Environment Config files (.env)");
  }

}

init();