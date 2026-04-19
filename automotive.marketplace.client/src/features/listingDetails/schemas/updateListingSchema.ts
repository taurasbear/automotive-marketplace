import { CreateListingSchema } from "@/features/createListing";
import z from "zod";

export const UpdateListingSchema = z.object({
  price: CreateListingSchema.shape.price,
  description: CreateListingSchema.shape.description,
  colour: CreateListingSchema.shape.colour,
  vin: CreateListingSchema.shape.vin,
  powerKw: CreateListingSchema.shape.powerKw.unwrap(),
  engineSizeMl: CreateListingSchema.shape.engineSizeMl.unwrap(),
  mileage: CreateListingSchema.shape.mileage,
  isSteeringWheelRight: CreateListingSchema.shape.isSteeringWheelRight,
  city: CreateListingSchema.shape.city,
  isUsed: CreateListingSchema.shape.isUsed,
  year: CreateListingSchema.shape.year,
});
