import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import { useQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { getUserContractProfileOptions } from "../api/getUserContractProfileOptions";

type VehicleDefaults = {
  make: string;
  commercialName: string;
  vin: string | null;
  mileage: number;
  price?: number | null;
};

type ContractFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  contractCardId: string;
  isSeller: boolean;
  isReadOnly?: boolean;
  submittedAt?: string | null;
  vehicleDefaults: VehicleDefaults;
  userEmail: string;
  onSubmitSeller: (
    cardId: string,
    data: SellerFormData,
    updateProfile: boolean,
  ) => void;
  onSubmitBuyer: (
    cardId: string,
    data: BuyerFormData,
    updateProfile: boolean,
  ) => void;
};

export type SellerFormData = {
  sdkCode?: string;
  make: string;
  commercialName: string;
  registrationNumber: string;
  mileage: number;
  vin?: string;
  registrationCertificate?: string;
  technicalInspectionValid: boolean;
  wasDamaged: boolean;
  damageKnown?: boolean;
  defectBrakes: boolean;
  defectSafety: boolean;
  defectSteering: boolean;
  defectExhaust: boolean;
  defectLighting: boolean;
  defectDetails?: string;
  price?: number;
  personalIdCode: string;
  fullName: string;
  phone: string;
  email: string;
  address: string;
  country: string;
};

export type BuyerFormData = {
  personalIdCode: string;
  fullName: string;
  phone: string;
  email: string;
  address: string;
};

const ContractFormDialog = ({
  open,
  onOpenChange,
  contractCardId,
  isSeller,
  isReadOnly = false,
  submittedAt,
  vehicleDefaults,
  userEmail,
  onSubmitSeller,
  onSubmitBuyer,
}: ContractFormDialogProps) => {
  const { t } = useTranslation("chat");
  const locale = useDateLocale();

  const { data: profileData } = useQuery(getUserContractProfileOptions());
  const profile = profileData?.data;

  const [step, setStep] = useState<1 | 2>(isSeller ? 1 : 2);
  const [updateProfile, setUpdateProfile] = useState(false);

  // Step 1 — Vehicle fields
  const [make, setMake] = useState(vehicleDefaults.make);
  const [commercialName, setCommercialName] = useState(
    vehicleDefaults.commercialName,
  );
  const [vin, setVin] = useState(vehicleDefaults.vin ?? "");
  const [mileage, setMileage] = useState(String(vehicleDefaults.mileage));
  const [price, setPrice] = useState(
    vehicleDefaults.price ? String(Math.round(vehicleDefaults.price)) : "",
  );

  useEffect(() => {
    if (vehicleDefaults.price && !price) {
      setPrice(String(Math.round(vehicleDefaults.price)));
    }
  }, [vehicleDefaults.price]);
  const [registrationNumber, setRegistrationNumber] = useState("");
  const [sdkCode, setSdkCode] = useState("");
  const [registrationCertificate, setRegistrationCertificate] = useState("");
  const [technicalInspectionValid, setTechnicalInspectionValid] =
    useState(true);
  const [wasDamaged, setWasDamaged] = useState(false);
  const [damageKnown, setDamageKnown] = useState<boolean | undefined>(
    undefined,
  );
  const [defectBrakes, setDefectBrakes] = useState(false);
  const [defectSafety, setDefectSafety] = useState(false);
  const [defectSteering, setDefectSteering] = useState(false);
  const [defectExhaust, setDefectExhaust] = useState(false);
  const [defectLighting, setDefectLighting] = useState(false);
  const [defectDetails, setDefectDetails] = useState("");

  // Step 2 — Personal fields
  const [personalIdCode, setPersonalIdCode] = useState(
    profile?.personalIdCode ?? "",
  );
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState(profile?.phoneNumber ?? "");
  const [address, setAddress] = useState(profile?.address ?? "");
  const [country, setCountry] = useState("Lietuva");

  const disabled = isReadOnly;

  const handleSubmit = () => {
    if (isSeller) {
      onSubmitSeller(
        contractCardId,
        {
          sdkCode: sdkCode || undefined,
          make,
          commercialName,
          registrationNumber,
          mileage: Number(mileage),
          vin: vin || undefined,
          registrationCertificate: registrationCertificate || undefined,
          technicalInspectionValid,
          wasDamaged,
          damageKnown: wasDamaged ? damageKnown : undefined,
          defectBrakes,
          defectSafety,
          defectSteering,
          defectExhaust,
          defectLighting,
          defectDetails: defectDetails || undefined,
          price: price ? Number(price) : undefined,
          personalIdCode,
          fullName,
          phone,
          email: userEmail,
          address,
          country,
        },
        updateProfile,
      );
    } else {
      onSubmitBuyer(
        contractCardId,
        { personalIdCode, fullName, phone, email: userEmail, address },
        updateProfile,
      );
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-[560px]">
        <DialogHeader>
          <DialogTitle>
            {isReadOnly
              ? t("contractForm.titleReadOnly", {
                  date: submittedAt
                    ? format(new Date(submittedAt), "MMM d, yyyy", { locale })
                    : "",
                })
              : t("contractForm.title")}
          </DialogTitle>
        </DialogHeader>

        {/* Step indicator (seller only) */}
        {isSeller && !isReadOnly && (
          <div className="text-muted-foreground mb-2 flex items-center gap-2 text-xs">
            <span className={step === 1 ? "text-foreground font-semibold" : ""}>
              {t("contractForm.step1")}
            </span>
            <span>→</span>
            <span className={step === 2 ? "text-foreground font-semibold" : ""}>
              {t("contractForm.step2")}
            </span>
          </div>
        )}

        {/* Step 1 — Vehicle */}
        {step === 1 && isSeller && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label className="text-xs">{t("contractForm.make")}</Label>
                <Input
                  value={make}
                  onChange={(e) => setMake(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">
                  {t("contractForm.commercialName")}
                </Label>
                <Input
                  value={commercialName}
                  onChange={(e) => setCommercialName(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.vin")}</Label>
                <Input
                  value={vin}
                  onChange={(e) => setVin(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.mileage")}</Label>
                <Input
                  type="number"
                  value={mileage}
                  onChange={(e) => setMileage(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.price")}</Label>
                <Input
                  type="number"
                  step="1"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">
                  {t("contractForm.registrationNumber")}
                </Label>
                <Input
                  value={registrationNumber}
                  onChange={(e) => setRegistrationNumber(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.sdkCode")}</Label>
                <Input
                  value={sdkCode}
                  onChange={(e) => setSdkCode(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">
                  {t("contractForm.registrationCertificate")}
                </Label>
                <Input
                  value={registrationCertificate}
                  onChange={(e) => setRegistrationCertificate(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
            </div>

            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Checkbox
                  id="techInspection"
                  checked={technicalInspectionValid}
                  onCheckedChange={(v) => setTechnicalInspectionValid(!!v)}
                  disabled={disabled}
                />
                <Label htmlFor="techInspection" className="text-sm">
                  {t("contractForm.technicalInspectionValid")}
                </Label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id="wasDamaged"
                  checked={wasDamaged}
                  onCheckedChange={(v) => {
                    setWasDamaged(!!v);
                    if (!v) setDamageKnown(undefined);
                  }}
                  disabled={disabled}
                />
                <Label htmlFor="wasDamaged" className="text-sm">
                  {t("contractForm.wasDamaged")}
                </Label>
              </div>
              {wasDamaged && (
                <div className="ml-6 flex items-center gap-2">
                  <Checkbox
                    id="damageKnown"
                    checked={damageKnown === true}
                    onCheckedChange={(v) => setDamageKnown(!!v)}
                    disabled={disabled}
                  />
                  <Label htmlFor="damageKnown" className="text-sm">
                    {t("contractForm.damageKnown")}
                  </Label>
                </div>
              )}
            </div>

            <div className="space-y-1">
              <Label className="text-xs">{t("contractForm.defects")}</Label>
              <div className="grid grid-cols-2 gap-1">
                {(
                  [
                    ["defectBrakes", defectBrakes, setDefectBrakes],
                    ["defectSafety", defectSafety, setDefectSafety],
                    ["defectSteering", defectSteering, setDefectSteering],
                    ["defectExhaust", defectExhaust, setDefectExhaust],
                    ["defectLighting", defectLighting, setDefectLighting],
                  ] as const
                ).map(([key, val, setter]) => (
                  <div key={key} className="flex items-center gap-2">
                    <Checkbox
                      id={key}
                      checked={val}
                      onCheckedChange={(v) => setter(!!v)}
                      disabled={disabled}
                    />
                    <Label htmlFor={key} className="text-sm">
                      {t(`contractForm.${key}`)}
                    </Label>
                  </div>
                ))}
              </div>
            </div>

            <div>
              <Label className="text-xs">
                {t("contractForm.defectDetails")}
              </Label>
              <Textarea
                value={defectDetails}
                onChange={(e) => setDefectDetails(e.target.value)}
                disabled={disabled}
                className="min-h-[60px] text-sm"
              />
            </div>

            {!isReadOnly && (
              <Button className="w-full" onClick={() => setStep(2)}>
                {t("contractForm.next")}
              </Button>
            )}
          </div>
        )}

        {/* Step 2 — Personal */}
        {step === 2 && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <Label className="text-xs">{t("contractForm.email")}</Label>
                <Input
                  value={userEmail}
                  disabled
                  className="h-8 text-sm opacity-60"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.fullName")}</Label>
                <Input
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.phone")}</Label>
                <Input
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">
                  {t("contractForm.personalIdCode")}
                </Label>
                <Input
                  value={personalIdCode}
                  onChange={(e) => setPersonalIdCode(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                  placeholder={t("contractForm.personalIdCodePlaceholder")}
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.country")}</Label>
                <Input
                  value={country}
                  onChange={(e) => setCountry(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div className="col-span-2">
                <Label className="text-xs">{t("contractForm.address")}</Label>
                <Input
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
            </div>

            {!isReadOnly && (
              <>
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="updateProfile"
                    checked={updateProfile}
                    onCheckedChange={(v) => setUpdateProfile(!!v)}
                  />
                  <Label htmlFor="updateProfile" className="text-sm">
                    {t("contractForm.rememberForNextTime")}
                  </Label>
                </div>

                <div className="flex gap-2">
                  {isSeller && (
                    <Button
                      variant="outline"
                      className="flex-1"
                      onClick={() => setStep(1)}
                    >
                      {t("contractForm.back")}
                    </Button>
                  )}
                  <Button className="flex-1" onClick={handleSubmit}>
                    {t("contractForm.submit")}
                  </Button>
                </div>
              </>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default ContractFormDialog;
