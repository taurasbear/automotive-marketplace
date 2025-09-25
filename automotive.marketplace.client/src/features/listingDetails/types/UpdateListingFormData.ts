import z from "zod";
import { UpdateListingSchema } from "../schemas/updateListingSchema";

export type UpdateListingFormData = z.infer<typeof UpdateListingSchema>;
