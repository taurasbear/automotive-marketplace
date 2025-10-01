import z from "zod";
import { ListingSearchSchema } from "../schemas/listingSearchSchema";

export type ListingSearchParams = z.infer<typeof ListingSearchSchema>;
