import { useQuery } from "@tanstack/react-query"
import ListingService from "../services/ListingService"

export const useGetListings = () => {
    return useQuery({
        queryKey: ['listings'],
        queryFn: () => ListingService.GetListings(),
    })
}