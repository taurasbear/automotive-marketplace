import z from "zod";
import { CreateListingSchema } from "../schemas/createListingSchema";

export type CreateListingFormData = z.infer<typeof CreateListingSchema>;
