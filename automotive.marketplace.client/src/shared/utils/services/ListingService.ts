import axiosClient from "../../../api/axiosClient";
import { GetListingsDetailsWithCarResponse } from "../../types/dto/Listing/GetListingDetailsWithCarResponse";

class ListingService {
  static async GetListings(): Promise<GetListingsDetailsWithCarResponse> {
    const response = await axiosClient.get("/listing");
    return await response.data;
  }
}

export default ListingService;
