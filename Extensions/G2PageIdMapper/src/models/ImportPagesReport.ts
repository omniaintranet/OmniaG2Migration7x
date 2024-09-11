export interface ImportPagesReport {
    NumberOfSucceedItems: number;
    NumberOfFailedItems: number;
    SucceedItems: Array<ImportPagesItem | RefinedImportPagesItem>;
    FailedItems: Array<ImportPagesItem>;
    NewItems: Array<ImportPagesItem>;
    ReportName: string;
    StartedAt: string;
    FinishedAt: string;
    DurationInMinutes: number;
    Customer: string;
}

export interface ImportPagesItem {
    PageId: number;
    Path: string;
    NodeId: number;
    G1PhysicalPageUniqueId: string;
}

export interface RefinedImportPagesItem extends ImportPagesItem {
    G2PhysicalPageUniqueId: string;
}