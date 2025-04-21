import axiosClient from "../../../api/axiosClient";
import { GetListingDetailsWithCarResponse } from "../../types/dto/Listing/GetListingDetailsWithCarResponse";

class ListingService {
    static async GetListings(): Promise<GetListingDetailsWithCarResponse[]> {
        const response = await axiosClient.get('/listing');
        return await response.data.listingDetailsWithCar;
    }
}

export default ListingService;