import { Topic } from "../../DTOs/topic.dto";

export class AddUpdateUnitRequest {
    public UnitID: number;
    public HelpdeskID: number;
    public Name: string;
    public IsDeleted: boolean;
    public Code: string;
    public Topics: string[];

    constructor() {
        this.Topics = [];
        this.UnitID = 0;
    }
}