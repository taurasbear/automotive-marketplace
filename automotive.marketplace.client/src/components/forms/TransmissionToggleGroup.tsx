import { getAllTransmissionsOptions } from "@/api/enum/getAllTransmissionsOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { useTranslation } from "react-i18next";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type TransmissionToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const TransmissionToggleGroup = ({
  className,
  ...props
}: TransmissionToggleGroupProps) => {
  const { i18n } = useTranslation();
  const { data: transmissionsQuery } = useQuery(getAllTransmissionsOptions);

  const transmissions = transmissionsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {transmissions.map((transmission) => (
          <ToggleGroupItem
            key={transmission.id}
            value={transmission.id}
            className="flex-none"
          >
            {getTranslatedName(transmission.translations, i18n.language)}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default TransmissionToggleGroup;
