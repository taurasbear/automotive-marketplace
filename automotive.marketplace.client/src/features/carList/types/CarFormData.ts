import z from "zod";
import { CarFormSchema } from "../schemas/carFormSchema";

export type CarFormData = z.infer<typeof CarFormSchema>;
