import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { UpdateListingSchema } from "../schemas/updateListingSchema";
import { GetListingByIdResponse } from "../types/GetListingByIdResponse";
import { UpdateListingFormData } from "../types/UpdateListingFormData";

type EditListingFormProps = {
  id: string;
  listing: GetListingByIdResponse;
  onSubmit: (formData: UpdateListingFormData) => Promise<void>;
  className?: string;
};

const EditListingForm = ({
  listing,
  onSubmit,
  className,
}: EditListingFormProps) => {
  const { t } = useTranslation("listings");
  const form = useForm({
    resolver: zodResolver(UpdateListingSchema),
    defaultValues: {
      price: listing.price,
      description: listing.description,
      colour: listing.colour,
      vin: listing.vin,
      powerKw: listing.powerKw,
      engineSizeMl: listing.engineSizeMl,
      mileage: listing.mileage,
      isSteeringWheelRight: listing.isSteeringWheelRight,
      city: listing.city,
      isUsed: listing.isUsed,
      year: listing.year,
    },
  });

  console.log("form error: ", form.formState.errors);

  return (
    <div className={className}>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="grid w-full min-w-3xs grid-cols-4 space-y-4 gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
        >
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.year")}</FormLabel>
                <FormControl>
                  <Input type="number" min={1900} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="price"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.carPrice")}</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="powerKw"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.enginePowerKw")}</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="city"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.city")}</FormLabel>
                <FormControl>
                  <Input
                    placeholder={t("form.cityPlaceholder")}
                    type="text"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="description"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-4 flex flex-col justify-start">
                <FormLabel>{t("form.descriptionLabel")}</FormLabel>
                <FormControl>
                  <Textarea
                    className="max-h-96"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="vin"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.vinLabel")}</FormLabel>
                <FormControl>
                  <Input
                    placeholder={t("form.vinPlaceholder")}
                    type="text"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="engineSizeMl"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.engineSizeMl")}</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="mileage"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.mileage")}</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="colour"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("form.colour")}</FormLabel>
                <FormControl>
                  <Input
                    placeholder={t("form.colourPlaceholder")}
                    type="text"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isSteeringWheelRight"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col items-center justify-start">
                <FormLabel>{t("form.steeringWheelRight")}</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isUsed"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col items-center justify-start">
                <FormLabel>{t("form.usedCar")}</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button className="col-span-2 col-start-2" type="submit">
            {t("edit.saveChanges")}
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default EditListingForm;
